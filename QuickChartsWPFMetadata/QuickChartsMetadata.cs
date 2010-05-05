using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design;
using System.ComponentModel;
using Microsoft.Windows.Design.PropertyEditing;

#if BLEND
[assembly: ProvideMetadata(typeof(AmCharts.Windows.QuickCharts.Design.QuickChartsMetadata))]
#endif

namespace AmCharts.Windows.QuickCharts.Design
{
    public class QuickChartsMetadata :
#if BLEND
        IProvideAttributeTable
#else
        IRegisterMetadata
#endif
    {
        public AttributeTable AttributeTable
        {
            get
            {
                AttributeTableBuilder builder = new AttributeTableBuilder();

                AddDefaultPropertyAttributes(builder);

                AddToolboxBrowsableFalseAttributes(builder);

                AddNewItemTypesAttributes(builder);

                AddPropertyAttributes(builder);

                return builder.CreateTable();
            }
        }

        private static void AddPropertyAttributes(AttributeTableBuilder builder)
        {
            /// SerialChart
            builder.AddCustomAttributes(typeof(SerialChart), "Graphs",
                new CategoryAttribute("Graphs"));

            builder.AddCustomAttributes(typeof(SerialChart), "DataSource",
                new CategoryAttribute("Data"));

            builder.AddCustomAttributes(typeof(SerialChart), "CategoryValuePath",
                new CategoryAttribute("Data"));

            builder.AddCustomAttributes(typeof(SerialChart), "ValueFormatString",
                new CategoryAttribute("Data"));

            builder.AddCustomAttributes(typeof(SerialChart), "PresetBrushes",
                new CategoryAttribute("Brushes"));

            builder.AddCustomAttributes(typeof(SerialChart), "PlotAreaBackground",
                new CategoryAttribute("Brushes"));

            builder.AddCustomAttributes(typeof(SerialChart), "AxisForeground",
                new CategoryAttribute("Brushes"));

            builder.AddCustomAttributes(typeof(SerialChart), "MinimumValueGridStep",
                new CategoryAttribute("Grid"));

            builder.AddCustomAttributes(typeof(SerialChart), "MinimumCategoryGridStep",
                new CategoryAttribute("Grid"));

            builder.AddCustomAttributes(typeof(SerialChart), "LegendVisibility",
                new CategoryAttribute("Appearance"));

            /// SerialGraph
            builder.AddCustomAttributes(typeof(SerialGraph), "ValueMemberPath",
                new CategoryAttribute("Data"));

            builder.AddCustomAttributes(typeof(SerialGraph), "Title",
                new CategoryAttribute("Data"));

            builder.AddCustomAttributes(typeof(SerialGraph), "Brush",
                new CategoryAttribute("Brushes"));

            /// LineGraph
            builder.AddCustomAttributes(typeof(LineGraph), "StrokeThickness",
                new CategoryAttribute("Appearance"));

            /// ColumnGraph
            builder.AddCustomAttributes(typeof(ColumnGraph), "ColumnWidthAllocation",
                new CategoryAttribute("Appearance"));

            /// Pie Chart
            builder.AddCustomAttributes(typeof(PieChart), "DataSource",
                new CategoryAttribute("Data"));

            builder.AddCustomAttributes(typeof(PieChart), "TitleMemberPath",
                new CategoryAttribute("Data"));

            builder.AddCustomAttributes(typeof(PieChart), "ValueMemberPath",
                new CategoryAttribute("Data"));

            builder.AddCustomAttributes(typeof(PieChart), "LegendVisibility",
                new CategoryAttribute("Appearance"));

        }

        private static void AddNewItemTypesAttributes(AttributeTableBuilder builder)
        {
            builder.AddCustomAttributes(typeof(SerialChart), "Graphs",
                new NewItemTypesAttribute(
                    typeof(LineGraph),
                    typeof(AreaGraph),
                    typeof(ColumnGraph)));

            builder.AddCustomAttributes(typeof(SerialChart), "PresetBrushes",
                new NewItemTypesAttribute(typeof(System.Windows.Media.SolidColorBrush),
                    typeof(System.Windows.Media.LinearGradientBrush),
                    typeof(System.Windows.Media.RadialGradientBrush),
                    typeof(System.Windows.Media.ImageBrush)));

            builder.AddCustomAttributes(typeof(PieChart), "Brushes",
                new NewItemTypesAttribute(typeof(System.Windows.Media.SolidColorBrush),
                    typeof(System.Windows.Media.LinearGradientBrush),
                    typeof(System.Windows.Media.RadialGradientBrush),
                    typeof(System.Windows.Media.ImageBrush)));
        }

        private static void AddDefaultPropertyAttributes(AttributeTableBuilder builder)
        {
#if !SILVERLIGHT
            builder.AddCustomAttributes(
                typeof(SerialChart),
                new DefaultPropertyAttribute("Graphs")
                );

            builder.AddCustomAttributes(
                typeof(SerialGraph),
                new DefaultPropertyAttribute("ValueMemberPath")
                );
#endif
        }

        private static void AddToolboxBrowsableFalseAttributes(AttributeTableBuilder builder)
        {
            builder.AddCustomAttributes(
                typeof(SerialGraph),
                new ToolboxBrowsableAttribute(false)
                );

            builder.AddCustomAttributes(
                typeof(LineGraph),
                new ToolboxBrowsableAttribute(false)
                );

            builder.AddCustomAttributes(
                typeof(ColumnGraph),
                new ToolboxBrowsableAttribute(false)
                );

            builder.AddCustomAttributes(
                typeof(AreaGraph),
                new ToolboxBrowsableAttribute(false)
                );

            builder.AddCustomAttributes(
                typeof(CategoryAxis),
                new ToolboxBrowsableAttribute(false)
                );

            builder.AddCustomAttributes(
                typeof(ValueAxis),
                new ToolboxBrowsableAttribute(false)
                );

            builder.AddCustomAttributes(
                typeof(ValueGrid),
                new ToolboxBrowsableAttribute(false)
                );


            builder.AddCustomAttributes(
                typeof(Indicator),
                new ToolboxBrowsableAttribute(false)
                );


            builder.AddCustomAttributes(
                typeof(Legend),
                new ToolboxBrowsableAttribute(false)
                );

            builder.AddCustomAttributes(
                typeof(Slice),
                new ToolboxBrowsableAttribute(false)
                );
        }

#if !BLEND
        #region IRegisterMetadata Members

        public void Register()
        {
            MetadataStore.AddAttributeTable(AttributeTable);
        }

        #endregion
#endif
    }
}
