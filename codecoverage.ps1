# Очистка и пересборка
dotnet clean
dotnet build -c Debug

# Запуск тестов с покрытием
dotnet test -f net8.0 --no-build --collect:"XPlat Code Coverage"
#dotnet test -f net9.0 --no-build --collect:"XPlat Code Coverage"

# Поиск всех coverage.cobertura.xml
$coverageFiles = Get-ChildItem -Recurse -Filter coverage.cobertura.xml | ForEach-Object { $_.FullName }
$reportList = $coverageFiles -join ";"

# Генерация отчета
reportgenerator `
  -reports:$reportList `
  -targetdir:"coveragereport" `
  -classfilters:-System.Text.RegularExpressions.* `
  -reporttypes:HtmlInline_AzurePipelines `
  -verbosity:Info

# Открытие отчета
Start-Process "coveragereport\index.html"
