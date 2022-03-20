# grafana-json-to-jsonnet-converter

Simple, dirty, attempt to convert grafana exported JSON to Jsonnet.

It can be further improved and made more dynamic. This library is more or less a quick attempt to get it done.

**Usage Instructions:**
- Create a new folder `converting` (or what ever you like).
- Download the [grafonnet-lib](https://github.com/grafana/grafonnet-lib) into `converting`.
- Download the [GO JSON NET TOOL](https://github.com/google/go-jsonnet/releases).
- Change the Grafana JSON file location in Main, and run the library code.
- Copy the output, and create a file `input.jsonnet` in `converting` and paste the output.
- Run: `jsonnet.exe -J grafonnet-lib  input.jsonnet` (executing the jsonnet command differs per platform ofcourse). The `J` argument tells the tool were to find the includes in input.jsonnet (`converting/grafonet-lib`).
- If there are no syntactical erros, then the tool generates valid grafana JSON. Use this in grafana to import the dashboard. =

**Notes:**
- Generated DTO's from Grafana exporterd JSON ([see json2csharp](https://json2csharp.com/)).
- Looked at the [API-docs](https://grafana.github.io/grafonnet-lib/api-docs/) to find out what minimal fields are expected, and applicable to me. You might need toe extend the code a bit, add some fields.