To allow traffic needed by calling by other application, is able to use docker swarm to scale this application as a service in docker and using some tools like CHEF, made auto scalling.
the commands needed are:

---create an image by this application
docker build -t crossblog-img -f dockerfile .

---start an container with image generated
docker run --name crossblog -d -p 5000:5000 -t crossblog-img

---start docker as a service
docker service create --publish 5000:5000 --name crossblog crossblog-img

--scale up the application
docker service scale crossblog=5

--scale down
docker service scale crossblog=0

