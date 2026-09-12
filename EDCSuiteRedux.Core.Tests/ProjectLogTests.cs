using System.IO;
using Xunit;

namespace EDCSuiteRedux.Core.Tests
{
    public class ProjectLogTests
    {
        [Fact]
        public void WriteLogbookEntryCreatesProjectLog()
        {
            var projectFolder = Path.Combine(Path.GetTempPath(), "EDCSuiteRedux.Core.Tests", Path.GetRandomFileName());
            Directory.CreateDirectory(projectFolder);

            try
            {
                var log = new ProjectLog();
                log.OpenProjectLog(projectFolder);
                log.WriteLogbookEntry(LogbookEntryType.Note, "test|entry");

                var logPath = Path.Combine(projectFolder, "ProjectLogbook.log");
                var contents = File.ReadAllText(logPath);
                Assert.Contains("A project note was inserted", contents);
                Assert.Contains("test entry", contents);
            }
            finally
            {
                Directory.Delete(projectFolder, true);
            }
        }
    }
}
