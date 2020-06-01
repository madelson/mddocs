﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Grynwald.MdDocs.ApiReference.Configuration;
using Grynwald.MdDocs.Common.Configuration;
using Grynwald.Utilities.Configuration;
using Grynwald.Utilities.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Grynwald.MdDocs.ApiReference.Test.Configuration
{
    public class ApiReferenceConfigurationTest : IDisposable
    {
        private readonly TemporaryDirectory m_ConfigurationDirectory = new TemporaryDirectory();
        private readonly string m_ConfigurationFilePath;

        public ApiReferenceConfigurationTest()
        {
            m_ConfigurationFilePath = Path.Combine(m_ConfigurationDirectory, "mddocs.settings.json");

            // clear environment variables (might be set by previous test runs)
            var envVars = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);
            foreach (var key in envVars.Keys.Cast<string>().Where(x => x.StartsWith("MDDOCS__")))
            {
                Environment.SetEnvironmentVariable(key, null, EnvironmentVariableTarget.Process);
            }
        }

        public void Dispose() => m_ConfigurationDirectory.Dispose();

        private void PrepareConfiguration(string key, object value)
        {
            var configRoot = new JObject();

            var currentConfigObject = new JObject();
            configRoot.Add(new JProperty("mddocs", currentConfigObject));

            var keySegments = key.Split(":");
            for (var i = 0; i < keySegments.Length; i++)
            {
                // last fragment => add value
                if (i == keySegments.Length - 1)
                {
                    if (value.GetType().IsArray)
                    {
                        value = JArray.FromObject(value);
                    }

                    currentConfigObject.Add(new JProperty(keySegments[i], value));

                }
                // create child configuration object
                else
                {
                    var newConfigObject = new JObject();
                    currentConfigObject.Add(new JProperty(keySegments[i], newConfigObject));
                    currentConfigObject = newConfigObject;
                }
            }

            var json = configRoot.ToString(Formatting.Indented);
            File.WriteAllText(m_ConfigurationFilePath, json);
        }


        /// <summary>
        /// Gets the assertions that must be true for the default configuration
        /// </summary>
        public static IEnumerable<object[]> DefaultConfigAssertions()
        {
            static object[] TestCase(Action<ApiReferenceConfiguration> assertion)
            {
                return new[] { assertion };
            }

            yield return TestCase(config => Assert.NotNull(config));
            yield return TestCase(config => Assert.Empty(config.OutputPath));
            yield return TestCase(config => Assert.Empty(config.AssemblyPath));
            yield return TestCase(config => Assert.Equal(MarkdownPreset.Default, config.MarkdownPreset));

        }

        [Theory]
        [MemberData(nameof(DefaultConfigAssertions))]
        public void Default_configuration_is_valid(Action<ApiReferenceConfiguration> assertion)
        {
            var defaultConfig = new ConfigurationProvider().GetDefaultApiReferenceConfiguration();
            assertion(defaultConfig);
        }

        [Theory]
        [MemberData(nameof(DefaultConfigAssertions))]
        public void Empty_configuration_is_valid(Action<ApiReferenceConfiguration> assertion)
        {
            // ARRANGE           
            File.WriteAllText(m_ConfigurationFilePath, "{ }");

            // ACT
            var provider = new ConfigurationProvider(m_ConfigurationFilePath);
            var config = provider.GetApiReferenceConfiguration();

            // ASSERT
            assertion(config);
        }

        [Theory]
        [MemberData(nameof(DefaultConfigAssertions))]
        public void GetConfiguration_returns_default_configuration_if_config_file_does_not_exist(Action<ApiReferenceConfiguration> assertion)
        {
            var provider = new ConfigurationProvider(m_ConfigurationFilePath);
            var config = provider.GetApiReferenceConfiguration();
            assertion(config);
        }

        [Theory]
        [MemberData(nameof(DefaultConfigAssertions))]
        public void GetConfiguration_returns_default_configuration_if_config_file_path_is_empty(Action<ApiReferenceConfiguration> assertion)
        {
            var provider = new ConfigurationProvider(m_ConfigurationFilePath);
            var config = provider.GetApiReferenceConfiguration();
            assertion(config);
        }

        [Fact]
        public void OutputPath_can_be_set_in_configuration_file()
        {
            // ARRANGE            
            PrepareConfiguration("apireference:outputpath", @"C:\some-path");

            // ACT
            var provider = new ConfigurationProvider(m_ConfigurationFilePath);
            var config = provider.GetApiReferenceConfiguration();

            // ASSERT
            Assert.NotNull(config);
            Assert.Equal(@"C:\some-path", config.OutputPath);
        }

        private class TestClass1
        {
            [ConfigurationValue("mddocs:apireference:outputpath")]
            public string? OutputPath { get; set; }
        }

        [Fact]
        public void OutputPath_can_be_set_through_settings_object()
        {
            // ARRANGE            
            var settings = new TestClass1() { OutputPath = @"C:\some-path" };

            // ACT
            var provider = new ConfigurationProvider(m_ConfigurationFilePath, settings);
            var config = provider.GetApiReferenceConfiguration();

            // ASSERT
            Assert.Equal(@"C:\some-path", config.OutputPath);
        }


        [Fact]
        public void AssemblyPath_can_be_set_in_configuration_file()
        {
            // ARRANGE            
            PrepareConfiguration("apireference:assemblyPath", @"C:\some-path");

            // ACT
            var provider = new ConfigurationProvider(m_ConfigurationFilePath);
            var config = provider.GetApiReferenceConfiguration();

            // ASSERT
            Assert.NotNull(config);
            Assert.Equal(@"C:\some-path", config.AssemblyPath);
        }

        private class TestClass2
        {
            [ConfigurationValue("mddocs:apireference:assemblyPath")]
            public string? AssemblyPath { get; set; }
        }

        [Fact]
        public void AssemblyPath_can_be_set_through_settings_object()
        {
            // ARRANGE            
            var settings = new TestClass2() { AssemblyPath = @"C:\some-path" };

            // ACT
            var provider = new ConfigurationProvider(m_ConfigurationFilePath, settings);
            var config = provider.GetApiReferenceConfiguration();

            // ASSERT
            Assert.Equal(@"C:\some-path", config.AssemblyPath);
        }

        [Theory]
        [CombinatorialData]
        public void Markdown_preset_can_be_set_in_configuration_file(MarkdownPreset preset)
        {
            // ARRANGE            
            PrepareConfiguration("apireference:markdownPreset", preset.ToString());

            // ACT
            var provider = new ConfigurationProvider(m_ConfigurationFilePath);
            var config = provider.GetApiReferenceConfiguration();

            // ASSERT
            Assert.NotNull(config);
            Assert.Equal(preset, config.MarkdownPreset);
        }

        private class TestClass3
        {
            [ConfigurationValue("mddocs:apireference:markdownPreset")]
            public string? Preset { get; set; }
        }

        [Theory]
        [CombinatorialData]
        public void Markdown_preset_can_be_set_through_settings_object(MarkdownPreset preset)
        {
            // ARRANGE            
            var settings = new TestClass3() { Preset = preset.ToString() };

            // ACT
            var provider = new ConfigurationProvider(m_ConfigurationFilePath, settings);
            var config = provider.GetApiReferenceConfiguration();

            // ASSERT
            Assert.NotNull(config);
            Assert.Equal(preset, config.MarkdownPreset);
        }

        [Fact]
        public void GetConfiguration_converts_the_output_path_to_a_full_path()
        {
            // ARRANGE
            var relativePath = "../some-relative-path";
            PrepareConfiguration("apireference:outputPath", relativePath);

            var expectedPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(m_ConfigurationFilePath)!, relativePath));

            // ACT
            var provider = new ConfigurationProvider(m_ConfigurationFilePath);
            var config = provider.GetApiReferenceConfiguration();

            // ASSERT
            Assert.NotNull(config);
            Assert.Equal(expectedPath, config.OutputPath);
        }

        [Fact]
        public void GetConfiguration_does_not_change_the_output_path_if_value_is_a_rooted_path()
        {
            // ARRANGE
            var path = @"C:\some-path";
            PrepareConfiguration("apireference:outputPath", path);

            // ACT
            var provider = new ConfigurationProvider(m_ConfigurationFilePath);
            var config = provider.GetApiReferenceConfiguration();

            // ASSERT
            Assert.NotNull(config);
            Assert.Equal(path, config.OutputPath);
        }

        [Fact]
        public void GetConfiguration_converts_the_assembly_path_to_a_full_path()
        {
            // ARRANGE
            var relativePath = "../some-relative-path";
            PrepareConfiguration("apireference:assemblyPath", relativePath);

            var expectedPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(m_ConfigurationFilePath)!, relativePath));

            // ACT
            var provider = new ConfigurationProvider(m_ConfigurationFilePath);
            var config = provider.GetApiReferenceConfiguration();

            // ASSERT
            Assert.NotNull(config);
            Assert.Equal(expectedPath, config.AssemblyPath);
        }

        [Fact]
        public void GetConfiguration_does_not_change_the_assembly_path_if_value_is_a_rooted_path()
        {
            // ARRANGE
            var path = @"C:\some-path";
            PrepareConfiguration("apireference:assemblyPath", path);

            // ACT
            var provider = new ConfigurationProvider(m_ConfigurationFilePath);
            var config = provider.GetApiReferenceConfiguration();

            // ASSERT
            Assert.NotNull(config);
            Assert.Equal(path, config.AssemblyPath);
        }
    }
}
