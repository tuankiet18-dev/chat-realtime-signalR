FROM node:22-alpine AS dev
WORKDIR /app

COPY ChatApp.UI/package*.json ./
RUN npm ci

COPY ChatApp.UI/ ./

ENV VITE_API_BASE_URL=http://localhost:5185
EXPOSE 5173

CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0", "--port", "5173"]

FROM node:22-alpine AS build
WORKDIR /app

COPY ChatApp.UI/package*.json ./
RUN npm ci

COPY ChatApp.UI/ ./
ARG VITE_API_BASE_URL=http://localhost:5185
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm run build

FROM nginx:1.29-alpine AS final
COPY ChatApp.UI/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
