FROM node:latest as build

WORKDIR /usr/local/app
COPY ./ /usr/local/app

RUN npm install
RUN npm run build
RUN ls /usr/local/app/dist

FROM nginx:latest
COPY --from=build /usr/local/app/dist /usr/share/nginx/html

EXPOSE 80
