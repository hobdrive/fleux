using System;
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
            try
            {
                FleuxApplication.DeviceDpi = 100;

            }
            catch (Exception)
            {
            }
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

        [Test]
        public void horizontal_stack_panel_should_have_desired_size_when_autoResize_is_true()
        {
            var stackPanel = new StackPanel(false) {AutoResize = true, Size = new Size(500, 25)};

            var child1 = new TextElement("someText 1") { Size = new Size(4, 100) };
            var child2 = new TextElement("someText 2") { Size = new Size(55, 100) };
            var child3 = new TextElement("someText 3") { Size = new Size(333, 100) };

            stackPanel.AddElement(child1);
            stackPanel.AddElement(child2);
            stackPanel.AddElement(child3);

            Assert.AreEqual(child1.Width + child2.Width + child3.Width, stackPanel.Width);
        }

        [Test]
        public void horizontal_stack_panel_should_have_desired_size_with_columns_when_autoResize_is_true()
        {
            var stackPanel = new StackPanel(false) {AutoResize = true, Size = new Size(500, 25),Columns = 2};

            var child1 = new TextElement("someText 1") { Size = new Size(18, 100) };
            var child2 = new TextElement("someText 2") { Size = new Size(100, 100) };
            var child3 = new TextElement("someText 3") { Size = new Size(100, 100) };

            stackPanel.AddElement(child1);
            stackPanel.AddElement(child2);
            stackPanel.AddElement(child3);

            Assert.AreEqual(child2.Width + child3.Width, stackPanel.Width);
        }

        [Test]
        public void vertical_stack_panel_should_have_desired_size_when_autoResize_is_true()
        {
            var stackPanel = new StackPanel(false) { AutoResize = true, Size = new Size(50, 250) ,IsVertical = true};

            var child1 = new TextElement("someText 1") { Size = new Size(100, 13) };
            var child2 = new TextElement("someText 2") { Size = new Size(100, 50) };
            var child3 = new TextElement("someText 3") { Size = new Size(100, 120) };

            stackPanel.AddElement(child1);
            stackPanel.AddElement(child2);
            stackPanel.AddElement(child3);

            Assert.AreEqual(child1.Height + child2.Height + child3.Height, stackPanel.Height);
        }

        [Test]
        public void vertical_stack_panel_should_have_desired_size_with_columns_when_autoResize_is_true()
        {
            var stackPanel = new StackPanel(false) { AutoResize = true, Size = new Size(50, 250), Columns = 2, IsVertical = true };

            var child1 = new TextElement("someText 1") { Size = new Size(100, 100) };
            var child2 = new TextElement("someText 2") { Size = new Size(100, 150) };
            var child3 = new TextElement("someText 3") { Size = new Size(100, 200) };

            stackPanel.AddElement(child1);
            stackPanel.AddElement(child2);
            stackPanel.AddElement(child3);

            Assert.AreEqual(child2.Height + child3.Height, stackPanel.Height);
        }
    }
}