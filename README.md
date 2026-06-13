# Unit Converter API

A RESTful ASP.NET Core Web API for converting numerical values between units of measurement.

## Supported Categories

| Category | Units |
|----------|-------|
| Length | `m`, `km`, `cm`, `mm`, `in`, `ft`, `yd`, `mi` |
| Temperature | `C`, `F`, `K` |
| Weight / Mass | `kg`, `g`, `mg`, `t`, `lb`, `oz` |

## How to Run Locally

**Prerequisites:** [.NET 9 SDK](https://dotnet.microsoft.com/download)

```bash
git clone <your-repo-url>
cd UnitConverter

dotnet run --project src/UnitConverter.Api
```

The API will start at `http://localhost:5000`. Open that URL in your browser to access the Swagger UI.

## API Endpoints

### `POST /api/convert`

Convert a value from one unit to another.

**Request body:**
```json
{
  "value": 100,
  "fromUnit": "km",
  "toUnit": "mi"
}
```

**Response:**
```json
{
  "inputValue": 100,
  "fromUnit": "km",
  "fromUnitName": "Kilometre",
  "outputValue": 62.137119223734,
  "toUnit": "mi",
  "toUnitName": "Mile",
  "category": "Length"
}
```

Returns `400 Bad Request` if a unit symbol is unknown or the two units belong to different categories.

---

### `GET /api/categories`

Returns all supported categories and their unit symbols.

## Design Decisions

**Affine conversion model** — each unit is stored as a multiplier and offset relative to a base unit (`base = (value + offset) × multiplier`). This single formula covers both linear units (length, weight) and offset-based units (temperature) without any special-casing. Adding new units in the future only requires adding a row of data.

**Units hardcoded in the service** — as required. The `IConversionService` interface keeps the controller decoupled from the data source, so if units ever need to come from a database, only the service implementation changes.
