[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/fund.html) [![Latest GitHub release](https://img.shields.io/github/v/release/hmlendea/exonyms-api)](https://github.com/hmlendea/exonyms-api/releases/latest)

# About

REST API for gathering exonyms for a given location, in latin script.

# Usage

## Building from source

Just run the following command:
```sh
dotnet build
```

## Running

Just run the following command:
```sh
dotnet run
```

## Example request

The following example will gather a list of exonyms for _Al Homceima_:
```sh
curl --insecure --request GET --location 'http://localhost:5000/Exonyms?wikiDataId=Q310350'
```
