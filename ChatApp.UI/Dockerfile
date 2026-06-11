FROM node:22-alpine
WORKDIR /app

COPY ChatApp.UI/package*.json ./
RUN npm ci

COPY ChatApp.UI/ ./

ENV VITE_API_BASE_URL=http://localhost:5185
EXPOSE 5173

CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0", "--port", "5173"]
