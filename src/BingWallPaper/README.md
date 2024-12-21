# wallpaper-database

微软必应首页每日一图接口: [https://cn.bing.com/](https://cn.bing.com/)

## 接口

接口地址：
```
https://luoyunchong.github.io/actions/src/BingWallPaper/Data/2024/12/21.json
```

请求方式：

```
https://luoyunchong.github.io/actions/src/BingWallPaper/Data/<year>/<month>/<day>.json
```

参数说明

| 参数 | 类型 | 说明 | 
| - | - | - | 
| year | str | 4位年份, 例如：2022 | 
| month | str | 2位月份, 例如：02、12 | 
| day | str | 2位日期, 例如：02、25 | 

例如

[https://luoyunchong.github.io/actions/src/BingWallPaper/Data/2024/12/21.json](https://luoyunchong.github.io/actions/src/BingWallPaper/Data/2024/12/21.json)

返回数据

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

- headline: 标题
- title: 图片标题
- description: 图片描述
- ultraHighDef: 超高清图片地址
- imageUrl: 图片地址
- imageWallpaper: 壁纸地址
- mainText: 主要内容
- copyRight: 版权信息
- quickFactMainText: 快速事实

## Thanks

- [wallpaper-database](https://github.com/mouday/wallpaper-database)
- [wallpaper](https://github.com/mouday/wallpaper)
