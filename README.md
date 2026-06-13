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
git clone https://github.com/Arpit08-PM/unit-converter-api.git
cd unit-converter-api
dotnet run --project src/UnitConverter.Api
```

Open `http://localhost:5000` in your browser to access the Swagger UI.

## API Endpoints

### `POST /api/convert`

```json
{
  "value": 100,
  "fromUnit": "km",
  "toUnit": "mi"
}
```

### `GET /api/categories`

Returns all supported categories and their units.

## Design Decisions

Units are hardcoded using an affine conversion model — each unit stores a multiplier and offset relative to a base unit. This single formula handles both linear units (length, weight) and offset-based units (temperature) without special-casing.
