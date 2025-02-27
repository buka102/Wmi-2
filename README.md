# wmi
WMI Coding Exercise


Change working dir to `Wmi`
```
cd Wmi
```

To create DB, from db level run:
```
docker-compose up db
```

To remove external volume, and reconstruct DB:
```
docker-compose down -v 
```


## Run Api and Db
```
docker-compose up
```

Web Api runs on port 8081