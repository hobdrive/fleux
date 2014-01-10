using System.Drawing;
using Fleux.Core;
using Fleux.UIElements;
using NUnit.Framework;

namespace FleuxTests
{
    [TestFixture]
    public class StackPanelTests
    {
        public StackPanelTests()
        {
            FleuxApplication.DeviceDpi = 100;
        }

        [SetUp]
        public void SetUp()
        {
            FleuxApplication.TargetDesignDpi = 100;
        }

        [Test]
        public void vertical_stack_panel_should_relayout_correct()
        {
            var stackPanel = new StackPanel(true);

            var child1 = new TextElement("someText 1") {Size = new Size(50, 75)};
            var child2 = new TextElement("someText 2") {Size = new Size(50, 75)};
            var child3 = new TextElement("someText 3") {Size = new Size(50, 75)};

            stackPanel.AddElement(child1);
            stackPanel.AddElement(child2);
            stackPanel.AddElement(child3);

            stackPanel.Size = new Size(100, 300);

            Assert.AreEqual(child1.Location.Y, 0);
            Assert.AreEqual(child2.Location.Y, child1.Height);
            Assert.AreEqual(child3.Location.Y, child1.Height + child2.Height);

            Assert.AreEqual(child1.Location.X, 0);
            Assert.AreEqual(child2.Location.X, 0);
            Assert.AreEqual(child3.Location.X, 0);
        }

        [Test]
        public void vertical_stack_panel_should_relayout_correct_with_columns()
        {
            var stackPanel = new StackPanel(true) {Columns = 2};

            var child1 = new TextElement("someText 1") { Size = new Size(70, 75) };
            var child2 = new TextElement("someText 2") { Size = new Size(70, 75) };
            var child3 = new TextElement("someText 3") { Size = new Size(70, 75) };

            stackPanel.AddElement(child1);
            stackPanel.AddElement(child2);
            stackPanel.AddElement(child3);

            stackPanel.Size = new Size(100, 300);
            var columnSize = stackPanel.Size.Width / stackPanel.Columns;

            Assert.AreEqual(child1.Location.Y, 0);
            Assert.AreEqual(child1.Location.X, 0);

            Assert.AreEqual(child2.Location.Y, 0);

            Assert.AreEqual(child2.Location.X, columnSize);

            Assert.AreEqual(child3.Location.X, 0);
            Assert.AreEqual(child3.Location.Y, child1.Height);
        }

        [Test]
        public void vertical_stack_panel_should_increase_height_for_children_if_need()
        {
            var stackPanel = new StackPanel(true);

            var child1 = new TextElement("someText 1") {Size = new Size(50, 75)};
            var child2 = new TextElement("someText 2") {Size = new Size(50, 75)};
            var child3 = new TextElement("someText 3") {Size = new Size(50, 75)};

            stackPanel.AddElement(child1);
            stackPanel.AddElement(child2);
            stackPanel.AddElement(child3);

            stackPanel.Size = new Size(100, 200);

            Assert.AreEqual(child1.Height + child2.Height + child3.Height, stackPanel.Size.Height);
        }

        [Test]
        public void horizontal_stack_panel_should_relayout_correct()
        {
            var stackPanel = new StackPanel(false);

            var child1 = new TextElement("someText 1") {Size = new Size(50, 75)};
            var child2 = new TextElement("someText 2") {Size = new Size(50, 75)};
            var child3 = new TextElement("someText 3") {Size = new Size(50, 75)};

            stackPanel.AddElement(child1);
            stackPanel.AddElement(child2);
            stackPanel.AddElement(child3);

            stackPanel.Size = new Size(300, 100);

            Assert.AreEqual(child1.Location.X, 0);
            Assert.AreEqual(child2.Location.X, child1.Width);
            Assert.AreEqual(child3.Location.X, child1.Width + child2.Width);

            Assert.AreEqual(child1.Location.Y, 0);
            Assert.AreEqual(child2.Location.Y, 0);
            Assert.AreEqual(child3.Location.Y, 0);
        }

        [Test]
        public void horizontal_stack_panel_should_relayout_correct_with_column()
        {
            var stackPanel = new StackPanel(false) {Columns = 2};

            var child1 = new TextElement("someText 1") { Size = new Size(50, 75) };
            var child2 = new TextElement("someText 2") { Size = new Size(50, 75) };
            var child3 = new TextElement("someText 3") { Size = new Size(50, 75) };

            stackPanel.AddElement(child1);
            stackPanel.AddElement(child2);
            stackPanel.AddElement(child3);

            stackPanel.Size = new Size(300, 100);
            var columnSize = stackPanel.Size.Height / stackPanel.Columns;

            Assert.AreEqual(child1.Location.Y, 0);
            Assert.AreEqual(child1.Location.X, 0);

            Assert.AreEqual(child2.Location.Y, columnSize);
            Assert.AreEqual(child2.Location.X, 0);

            Assert.AreEqual(child3.Location.X, child2.Width);
            Assert.AreEqual(child3.Location.Y, 0);
        }

        [Test]
        public void horizontal_stack_panel_should_increase_width_for_children_if_need()
        {
            var stackPanel = new StackPanel(false);

            var child1 = new TextElement("someText 1") {Size = new Size(100, 75)};
            var child2 = new TextElement("someText 2") {Size = new Size(100, 75)};
            var child3 = new TextElement("someText 3") {Size = new Size(100, 75)};

            stackPanel.AddElement(child1);
            stackPanel.AddElement(child2);
            stackPanel.AddElement(child3);

            stackPanel.Size = new Size(100, 200);

            Assert.AreEqual(child1.Width + child2.Width + child3.Width, stackPanel.Size.Width);
        }
    }
}