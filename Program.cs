// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;

namespace Converter
{
    enum PanelType
    {
        Row,
        PieChart,
        Timeseries,
        Stat,
        BarGauge,
        Gauge
    }

    static class Program
    {
        static void Main(string[] args)
        {
            var json = File.ReadAllText("./test.json");
            Root parsed = JsonConvert.DeserializeObject<Root>(json);

            JSONNETBuilder builder = new JSONNETBuilder();
            
            foreach (var panel in parsed.panels) {
                if (panel == null)
                {
                    Console.WriteLine("null panel");
                }
                else
                {
                    builder.AddPanel(panel);
                }
            }

            Console.WriteLine(builder.jsonNetString);
        }
    }

    class JSONNETBuilder
    {
        public string jsonNetString = "";

        public JSONNETBuilder(string dashboardTitle)
        {
            jsonNetString = @"
                local grafana = import 'grafonnet/grafana.libsonnet';

                local prometheus = grafana.prometheus;

                grafana.dashboard.new(
                  'World Server New',
                  schemaVersion=26,
                  editable=true,
                  refresh='5s',
                  time_from='now-1h',
                  time_to='now',
                  timepicker=grafana.timepicker.new(
                    refresh_intervals=['30s', '1m', '5m', '15m', '30m', '1h', '2h', '1d', '2d', '7d'],
                  ),
                  uid='arkadia-worldservers-new',
                  tags=[],
                )
             ";
        }

        public void AddPanel(Panel panel)
        {
            string gridpos = "{" + string.Format("h: {0}, w: {1}, x: {2}, y: {3}", panel.gridPos.h, panel.gridPos.w, panel.gridPos.x, panel.gridPos.y) + "}";

            PanelType panelType = ParsePanelType(panel.type);

            jsonNetString += $@"
            .addPanel(
              gridPos={gridpos},
              panel={CreatePanel(panel, panelType)},
              datasource=prometheus
            ";

            if (panel.targets != null)
            {
                foreach (var target in panel.targets)
                {
                    jsonNetString += $@"  {AddTarget(target)}";
                }
            }

            jsonNetString += ")";

        }

        public string AddTarget(Target target)
        {
            var fields = AddField("", "format", target.format);
            fields = AddField(fields, "legendFormat", target.legendFormat);
            fields = AddField(fields, "intervalFactor", target.intervalFactor);
            fields = AddField(fields, "hide", target.hide);
            fields = AddField(fields, "instant", target.instant);
            fields = AddField(fields, "interval", target.interval);


            return $@".addTarget(
                    prometheus.target(
                      expr ='{target.expr}',
                      {fields}
                    )
                  )
            ";
        }

        private string CreatePanel(Panel panel, PanelType panelType)
        {
            switch (panelType)
            {
                case PanelType.Row:
                    return $"{RowPanel(panel)}";
                case PanelType.PieChart:
                    return $"{PiePanel(panel)}";
                case PanelType.Timeseries:
                    return $"{TimeseriesPanel(panel)}";
                case PanelType.Stat:
                    return $"{StatsPanel(panel)}";
                case PanelType.BarGauge:
                    return $"{BarGauge(panel)}";
                case PanelType.Gauge:
                    return $"{Gauge(panel)}";
                default:
                    throw new Exception("No such panel can be created");
            }
        }

        private string RowPanel(Panel panel)
        {
            var fields = AddField("", "title", panel.title);
            fields = AddField(fields, "description", panel.description);
            fields = AddField(fields, "collapse", panel.collapsed ? "true" : "false");

            return @$"grafana.row.new({fields})";
        }


        private string TimeseriesPanel(Panel panel)
        {
            var config = panel.fieldConfig.defaults;
            var custom = config.custom;
            var color = config.color;

            var fields = AddField("", "title", panel.title);
            fields = AddField(fields, "description", panel.description);


            return @$"grafana.graphPanel.new({fields})";
        }

        private string Gauge(Panel panel)
        {
            var config = panel.fieldConfig.defaults;

            var fields = AddField("", "title", panel.title);
            fields = AddField(fields, "description", panel.description);
            fields = AddField(fields, "pluginVersion", panel.pluginVersion);
            fields = AddField(fields, "unit", config.unit);
            fields = AddField(fields, "thresholdsMode", config.thresholds.mode);

            return @$"grafana.gaugePanel.new({fields})";
        }

        private string BarGauge(Panel panel)
        {
            var config = panel.fieldConfig.defaults;

            var fields = AddField("", "title", panel.title);
            fields = AddField(fields, "description", panel.description);
            fields = AddField(fields, "unit", config.unit);

            return @$"grafana.barGaugePanel.new({fields})";
        }

        private string PiePanel(Panel panel)
        {
            var options = panel.options;
            var config = panel.fieldConfig.defaults;

            var fields = AddField("", "title", panel.title);
            fields = AddField(fields, "description", panel.description);
            fields = AddField(fields, "pieType", options.pieType);

            return @$"grafana.pieChartPanel.new({fields})";
        }

        private string StatsPanel(Panel panel)
        {
            var options = panel.options;
            var config = panel.fieldConfig.defaults;

            var fields = AddField("", "title", panel.title);
            fields = AddField(fields, "description", panel.description);
            fields = AddField(fields, "colorMode", options.colorMode);
            fields = AddField(fields, "justifyMode", options.justifyMode);
            fields = AddField(fields, "orientation", options.orientation);
            fields = AddField(fields, "pluginVersion", panel.pluginVersion);
            fields = AddField(fields, "unit", config.unit);
            fields = AddField(fields, "thresholdsMode", config.thresholds.mode);

            return @$"grafana.statPanel.new({fields})";
        }

        private string AddField(string json, string name, string? field)
        {
            if (!String.IsNullOrEmpty(field))
            {
                return json += @$"{name}='{field}',";
            }

            return json;
        }


        private string AddField(string json, string name, dynamic? field)
        {
            if (field != null)
            {
                return json += @$"{name}='{field}',";
            }

            return json;
        }


        private PanelType ParsePanelType(string type)
        {
            switch (type)
            {
                case "row":
                    return PanelType.Row;
                case "piechart":
                    return PanelType.PieChart;
                case "timeseries":
                    return PanelType.Timeseries;
                case "stat":
                    return PanelType.Stat;
                case "bargauge":
                    return PanelType.BarGauge;
                case "gauge":
                    return PanelType.Gauge;
                default:
                    throw new Exception("No such type exists");
            }
        }
    }
}