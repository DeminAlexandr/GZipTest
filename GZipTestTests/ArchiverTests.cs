using Microsoft.VisualStudio.TestTools.UnitTesting;
using GZipArchiver.Archiver;
using System.IO;

namespace GZipTestTests
{
    [Ignore]
    [TestClass]
    public class ArchiverTests
    {
        [TestMethod]
        public void TestCompress()
        {
            string inputFilePath = "W:\\Test\\test.pdf";
            string outputFilePath = "W:\\Test\\test.gz";
            var inputFile = new FileInfo(inputFilePath);
            Assert.IsTrue(inputFile.Exists);
            var archiver = new Archiver();
            archiver.PrepareToCompress(inputFilePath, outputFilePath);
            archiver.Start();
            var outputFile = new FileInfo(outputFilePath);
            Assert.IsTrue(outputFile.Exists);
            Assert.IsTrue(outputFile.Length > 0);
        }

        [TestMethod]
        public void TestDecompress()
        {
            string inputFilePath = "W:\\Test\\test.gz";
            string outputFilePath = "W:\\Test\\test.pdf";
            var inputFile = new FileInfo(inputFilePath);
            Assert.IsTrue(inputFile.Exists);
            var archiver = new Archiver();
            archiver.PrepareToDecompress(inputFilePath, outputFilePath);
            archiver.Start();
            var outputFile = new FileInfo(outputFilePath);
            Assert.IsTrue(outputFile.Exists);
            Assert.IsTrue(outputFile.Length > 0);
        }

        [TestMethod]
        public void TestCompressAndDecompress()
        {
            // compress
            string inputFilePath = "W:\\Test\\test.pdf";
            var inputFile = new FileInfo(inputFilePath);
            string outputFilePath = inputFile.Directory + "\\"
                + inputFile.Name.Remove(inputFile.Name.Length - inputFile.Extension.Length)
                + ".gz";
            var archiver = new Archiver();
            archiver.PrepareToCompress(inputFilePath, outputFilePath);
            archiver.Start();
            var outputFile = new FileInfo(outputFilePath);
            Assert.IsTrue(outputFile.Exists);
            Assert.IsTrue(outputFile.Length > 0);
            // decompress
            inputFilePath = outputFilePath;
            outputFilePath = outputFilePath.Remove(outputFilePath.Length - 3) + "_copy" + inputFile.Extension;
            archiver = new Archiver();
            archiver.PrepareToDecompress(inputFilePath, outputFilePath);
            archiver.Start();
            outputFile = new FileInfo(outputFilePath);
            Assert.IsTrue(outputFile.Exists);
            Assert.IsTrue(outputFile.Length == inputFile.Length);
        }
    }
}
