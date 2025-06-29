# Dockerfile for pion-fe (Angular)
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build -- --configuration=production --output-path=dist/pion-fe
RUN ls -l dist/pion-fe

FROM nginx:alpine
COPY --from=build /app/dist/pion-fe/browser /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
