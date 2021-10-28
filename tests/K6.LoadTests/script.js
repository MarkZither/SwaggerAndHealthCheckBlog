import { check } from "k6";
import { sleep } from "k6";
import http from "k6/http";

export const options = {
  ext: {
    loadimpact: {
      distribution: {
        "amazon:de:frankfurt": {
          loadZone: "amazon:de:frankfurt",
          percent: 100,
        },
      },
    },
  },
  stages: [
    { target: 20, duration: "1m" },
    { target: 20, duration: "3m30s" },
    { target: 0, duration: "1m" },
  ],
  thresholds: {},
};

export default function() {
    let res = http.get("https://test.loadimpact.com/");
    check(res, {
      "is status 200": (r) => r.status === 200
    });
    let response;

    // swagger
    response = http.get("http://localhost:1114/swagger/index.html");
    check(response, {
        "is status 200": (r) => r.status === 200
      });
    // Weather Forecast
    response = http.get("http://localhost:1114/weatherforecast");
    check(response, {
        "is status 200": (r) => r.status === 200
      });
    // Automatically added sleep
    sleep(1);
  };
  