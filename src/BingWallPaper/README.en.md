**English** | [简体中文](README.md)

﻿# wallpaper-database

Microsoft Bing daily homepage image API: [https://cn.bing.com/](https://cn.bing.com/)

## API

Example URL:
```
https://luoyunchong.github.io/actions/src/BingWallPaper/Data/2024/12/21.json
```

Request URL pattern:

```
https://luoyunchong.github.io/actions/src/BingWallPaper/Data/<year>/<month>/<day>.json
```

Parameters

| Parameter | Type | Description | 
| - | - | - | 
| year | str | Four-digit year, for example 2022 | 
| month | str | Two-digit month, for example 02 or 12 | 
| day | str | Two-digit day, for example 02 or 25 | 

Example:

[https://luoyunchong.github.io/actions/src/BingWallPaper/Data/2024/12/21.json](https://luoyunchong.github.io/actions/src/BingWallPaper/Data/2024/12/21.json)

Response data (example values are in Chinese)

```json
{
  "headline": "极致的冬日景色",
  "title": "满拉水库的雪景，日喀则，中国西藏自治区",
  "description": "冬至既有自然的内涵，也有人文的内涵。从自然角度看，冬至是二十四节气中的一个重要节气，冬至过后，我国各地气候进入最寒冷的阶段。从文化角度看，这一天也是中华民族的传统节日，全国各地都会举行不同的文化习俗来庆祝这个节气的到来。",
  "ultraHighDef": "https://s.cn.bing.net/th?id=OHR.WinterSolstice2024_ZH-CN2045153949_UHD.jpg",
  "imageUrl": "https://s.cn.bing.net/th?id=OHR.WinterSolstice2024_ZH-CN2045153949_1920x1080.webp",
  "imageWallpaper": "https://cn.bing.com/th?id=OHR.WinterSolstice2024_ZH-CN2045153949_1920x1200.jpg&rf=LaDigue_1920x1200.jpg",
  "mainText": "满拉水库位于江孜县龙马乡，被称为西藏第一坝。",
  "copyRight": "© Zhang Zhenqi/VCG via Getty Images",
  "quickFactMainText": "满拉水库位于江孜县龙马乡，被称为西藏第一坝。"
}
```

- headline: headline
- title: image title
- description: image description
- ultraHighDef: ultra-high-definition image URL
- imageUrl: image URL
- imageWallpaper: wallpaper URL
- mainText: main text
- copyRight: copyright
- quickFactMainText: quick facts

## Thanks

- [wallpaper-database](https://github.com/mouday/wallpaper-database)
- [wallpaper](https://github.com/mouday/wallpaper)
