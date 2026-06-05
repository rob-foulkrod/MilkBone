# Testing

This project uses xUnit for automated tests and Coverlet/XPlat code coverage for reporting.

## Running tests locally

From the repository root:

```bash
dotnet test TodoApp.Tests/TodoApp.Tests.csproj --settings coverlet.runsettings
```

## Running tests with coverage

To generate coverage output locally, use:

```bash
dotnet test TodoApp.Tests/TodoApp.Tests.csproj --collect:"XPlat Code Coverage" --settings coverlet.runsettings --results-directory ./TestResults
```

This produces coverage artifacts in the `TestResults` folder, including `coverage.cobertura.xml`, while excluding generated Razor/view coverage noise.

## Notes for CI/CD

The test project is already set up to work with coverage collection in a CI environment. The same `dotnet test ... --collect:"XPlat Code Coverage" --settings coverlet.runsettings` command can be used in automation pipelines, which also excludes generated Razor/view coverage noise.
