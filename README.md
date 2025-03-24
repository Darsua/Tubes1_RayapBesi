
# Robocode Tank Royale

**Robocode Tank Royale** adalah sebuah permainan untuk membuat algoritma Tank yang dapat bertahan hingga akhir permainan. Pada projek ini, kami membuat Bot Tank yang mengimplementasikan ***algoritma greedy*** untuk mendapatkan skor setinggi mungkin.


## Contributor

| NIM       | Nama |
|------------------|-------------|
| 13523009        | Muhammad Hazim Ramadhan Prajoda	|
|       13523052   | Adhimas Aryo Bimo |
| 13523061      | Darrel Adinarya Sunanda |


## Installation

Untuk melakukan instalasi pada project ini, Anda diperlukan untuk mengunduk .NET dan mengunduh file jar pada repository berikut:

https://github.com/Ariel-HS/tubes1-if2211-starter-pack/releases/tag/v1.0

Setelah melakukannya, Anda dapat melakukan clone pada repository ini dengan mengikuti command berikut:

```bash
  git clone https://github.com/Darsua/Tubes1_RayapBesi
  cd src
```

Untuk lebih mudah, pastikan file jar dengan clone berada pada folder yang sama.

Selanjutnya, anda dapat menjalankan file jar dengan melakukan klik 2x pada file tersebut atau menjalankannya dengan command berikut:

```bash
  java -jar robocode-tankroyale-gui-0.30.0.jar
```


    
## Load Bot

Setelah menjalankan program, Anda perlu menyesuaikan config sesuai dengan source dari bot Anda. Tambahkan directory dari folder ini pada config tersebut.


## Created Bot

- **FlankerD** : mengincar bagian belakang musuh

- **IkanLele** : mengunci pergerakan musuh

- **SuperSub** : mendekat dan menyerang ketika musuh sedikit

- **RimRunnerz** : menjaga jarak dari musuh



## Data Structure

``` 
TUBES1_RAYAPBESI
│── docs
│── src
│   └─ alternative-bots
│       │── FlankerD
│       │   │── bin
│       │   │── obj
│       │   │── FlankerD.cmd
│       │   │── FlankerD.cs
│       │   │── FlankerD.csproj
│       │   │── FlankerD.json
│       │   └─  FlankerD.sh
│       │
│       │── IkanLele
│       │   │── bin
│       │   │── obj
│       │   │── IkanLele.cmd
│       │   │── IkanLele.cs
│       │   │── IkanLele.csproj
│       │   │── IkanLele.json
│       │   └─ IkanLele.sh
│       │
│       │── RimRunnerZ
│       │   │── bin
│       │   │── obj
│       │   │── RimRunnerZ.cmd
│       │   │── RimRunnerZ.cs
│       │   │── RimRunnerZ.csproj
│       │   │── RimRunnerZ.json
│       │   └─ RimRunnerZ.sh
│       │
│       └─ SuperSub
│           │── bin
│           │── obj
│           │── SuperSub.cmd
│           │── SuperSub.cs
│           │── SuperSub.csproj
│           │── SuperSub.json
│           └─ SuperSub.sh
│
│── .gitignore
│── LICENSE
└─ README.md

```


## Lessons Learned

Dalam project ini kami menjadi lebih paham mengenai penggunaan algoritma greedy untuk menyelsaikan suatu persoalan. Algoritma ini dapat menjadi solusi walaupun belum tentu solusi yang diberikan merupakan solusi yang paling optimal.


## License

[MIT](https://choosealicense.com/licenses/mit/)

