// Copyright � 2009 Syterra Software Inc.
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License version 2.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

using System.Text;
using fitSharp.Fit.Model;
using fitSharp.Fit.Service;
using NUnit.Framework;

namespace fit.Test.NUnit {
    [TestFixture]
    public class XmlResultWriterTest
    {
        private const string TEST_RESULT_FILE_NAME = "Test.xml";
        private XmlResultWriter _strategy;
        private FolderTestModel _folderModel;

        [SetUp]
        public void SetUp()
        {
            _folderModel = new FolderTestModel();
        }

        [Test]
        public void TestCloseWithFileName()
        {
            _strategy = new XmlResultWriter(TEST_RESULT_FILE_NAME, _folderModel);
            _strategy.Close();
            Assert.IsTrue(_folderModel.FileExists(TEST_RESULT_FILE_NAME));
            Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<testResults />", _folderModel.FileContent(TEST_RESULT_FILE_NAME));
        }

        [Test]
        public void TestCloseWithStandardOut()
        {
            _strategy = new XmlResultWriter("stdout", _folderModel);
            _strategy.Close();
            Assert.IsFalse(_folderModel.FileExists(TEST_RESULT_FILE_NAME));
        }

        [Test]
        public void TestWriteResults()
        {
            const string pageName = "Test Page";
            var pageResult = new PageResult(pageName, "<table border=\"1\" cellspacing=\"0\">\r\n<tr><td>Text</td>\r\n</tr>\r\n</table>", TestUtils.MakeTestCounts());
            _strategy = new XmlResultWriter(TEST_RESULT_FILE_NAME, _folderModel);
            _strategy.WritePageResult(pageResult);
            _strategy.Close();
            Assert.AreEqual(
                BuildPageResultString(pageName, "<![CDATA[<table border=\"1\" cellspacing=\"0\">\r\n<tr><td>Text</td>\r\n</tr>\r\n</table>]]>", 1, 2, 3, 4),
                _folderModel.FileContent(TEST_RESULT_FILE_NAME));
        }

        [Test]
        public void TestWriteFinalCounts()
        {
            _strategy = new XmlResultWriter(TEST_RESULT_FILE_NAME, _folderModel);
            _strategy.WriteFinalCount(TestUtils.MakeTestCounts());
            _strategy.Close();
            Assert.AreEqual(BuildFinalCountsString(1, 2, 3, 4),
                            _folderModel.FileContent(TEST_RESULT_FILE_NAME));
        }

        private static string BuildPageResultString(string pageName, string content, int right, int wrong, int ignores, int exceptions)
        {
            var builder = new StringBuilder();
            builder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-16\"?>");
            builder.AppendLine("<testResults>");
            builder.AppendLine("  <result>");
            builder.AppendFormat("    <relativePageName>{0}</relativePageName>\r\n", pageName);
            builder.AppendFormat("    <content>{0}</content>\r\n", content);
            builder.AppendLine("    <counts>");
            builder.AppendFormat("      <right>{0}</right>\r\n", right);
            builder.AppendFormat("      <wrong>{0}</wrong>\r\n", wrong);
            builder.AppendFormat("      <ignores>{0}</ignores>\r\n", ignores);
            builder.AppendFormat("      <exceptions>{0}</exceptions>\r\n", exceptions);
            builder.AppendLine("    </counts>");
            builder.AppendLine("  </result>");
            builder.Append("</testResults>");
            return builder.ToString();
        }

        private static string BuildFinalCountsString(int right, int wrong, int ignores, int exceptions)
        {
            var builder = new StringBuilder();
            builder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-16\"?>");
            builder.AppendLine("<testResults>");
            builder.AppendLine("  <finalCounts>");
            builder.AppendFormat("    <right>{0}</right>\r\n", right);
            builder.AppendFormat("    <wrong>{0}</wrong>\r\n", wrong);
            builder.AppendFormat("    <ignores>{0}</ignores>\r\n", ignores);
            builder.AppendFormat("    <exceptions>{0}</exceptions>\r\n", exceptions);
            builder.AppendLine("  </finalCounts>");
            builder.Append("</testResults>");
            return builder.ToString();
        }
    }
}