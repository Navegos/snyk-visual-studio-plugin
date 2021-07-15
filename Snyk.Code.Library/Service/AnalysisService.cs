﻿namespace Snyk.Code.Library.Service
{
    using System;
    using System.Threading.Tasks;
    using Snyk.Code.Library.Api;
    using Snyk.Code.Library.Api.Dto.Analysis;
    using Snyk.Code.Library.Domain.Analysis;

    /// <inheritdoc/>
    public class AnalysisService : IAnalysisService
    {
        private ISnykCodeClient codeClient;

        public AnalysisService(ISnykCodeClient codeClient) => this.codeClient = codeClient;

        /// <inheritdoc/>
        public async Task<AnalysisResult> GetAnalysisAsync(string bundleId)
        {
            if (string.IsNullOrEmpty(bundleId))
            {
                throw new ArgumentException("Bundle id is null or empty.");
            }

            AnalysisResultDto analysisResultDto;

            do
            {
                analysisResultDto = await this.codeClient.GetAnalysisAsync(bundleId);

                if (analysisResultDto.Status == AnalysisStatus.Waiting)
                {
                    System.Threading.Thread.Sleep(3000);
                }

                if (analysisResultDto.Status == AnalysisStatus.Failed)
                {
                    throw new SnykCodeException("SnykCode Analysis failed.");
                }
            }
            while (analysisResultDto.Status != AnalysisStatus.Done);

            return this.MapDtoAnalysisResultToDomain(analysisResultDto);
        }

        private AnalysisResult MapDtoAnalysisResultToDomain(AnalysisResultDto analysisResultDto)
        {
            var analysisrResult = new AnalysisResult
            {
                Status = analysisResultDto.Status,
                Progress = analysisResultDto.Progress,
                URL = analysisResultDto.AnalysisURL,
            };

            var analysisResults = analysisResultDto.AnalysisResults;

            if (analysisResults == null)
            {
                return analysisrResult;
            }

            foreach (var fileKeyPair in analysisResults.Files)
            {
                var fileAnalysis = new FileAnalysis { FileName = fileKeyPair.Key, };

                foreach (var suggestionIdToFileKeyPair in fileKeyPair.Value)
                {
                    string suggestionId = suggestionIdToFileKeyPair.Key;
                    var fileDtos = suggestionIdToFileKeyPair.Value;

                    var suggestionDto = analysisResults.Suggestions[suggestionId];

                    var suggestion = new Suggestion
                    {
                        Id = suggestionDto.Id,
                        Rule = suggestionDto.Rule,
                        Message = suggestionDto.Message,
                        Severity = suggestionDto.Severity,
                        Categories = suggestionDto.Categories,
                        Tags = suggestionDto.Tags,
                        Title = suggestionDto.Title,
                        Cwe = suggestionDto.Cwe,
                        Text = suggestionDto.Text,
                        ExampleCommitDescriptions = suggestionDto.ExampleCommitDescriptions,
                    };

                    foreach (var exampleCommitFixes in suggestionDto.ExampleCommitFixes)
                    {
                        suggestion.Fixes.Add(new SuggestionFix
                        {
                            CommitURL = exampleCommitFixes.CommitURL,
                            Line = exampleCommitFixes.Lines[0].Line,
                            LineNumber = exampleCommitFixes.Lines[0].LineNumber,
                            LineChange = exampleCommitFixes.Lines[0].LineChange,
                        });
                    }

                    fileAnalysis.Suggestions.Add(suggestion);
                }

                analysisrResult.FileAnalyses.Add(fileAnalysis);
            }

            return analysisrResult;
        }
    }
}