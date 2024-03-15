import { http, HttpResponse } from 'msw';
import texts from '../data/texts.json';

export const handlers = [
	http.get('/localizedtext', () => {
		return HttpResponse.json(texts);
	}),
  http.get('backoffice/umbracoapi/icon/geticons', () => {
    return HttpResponse.json({
      "icon-google-fill": "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\"><path d=\"M3.064 7.51A9.996 9.996 0 0112 2c2.695 0 4.959.991 6.69 2.605l-2.867 2.868C14.786 6.482 13.468 5.977 12 5.977c-2.605 0-4.81 1.76-5.595 4.123-.2.6-.314 1.24-.314 1.9 0 .66.114 1.3.314 1.9.786 2.364 2.99 4.123 5.595 4.123 1.345 0 2.49-.355 3.386-.955a4.6 4.6 0 001.996-3.018H12v-3.868h9.418c.118.654.182 1.336.182 2.045 0 3.046-1.09 5.61-2.982 7.35C16.964 21.105 14.7 22 12 22A9.996 9.996 0 012 12c0-1.614.386-3.14 1.064-4.49z\"/></svg>",
    });
  }),
];
