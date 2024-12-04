import { HttpHandler, http, HttpResponse } from "msw";
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import logoUrl from '../../../../Umbraco.Cms.StaticAssets/wwwroot/umbraco/assets/logo_light.svg';
import logoAlternativeUrl from '../../../../Umbraco.Cms.StaticAssets/wwwroot/umbraco/assets/logo_dark.svg';
import backgroundUrl from '../../../../Umbraco.Cms.StaticAssets/wwwroot/umbraco/assets/login.jpg';

export const handlers: HttpHandler[] = [
	http.get(umbracoPath('/security/back-office/graphics/logo'), async () => {
		const imageBuffer = await fetch(logoUrl)
			.then((res) => res.arrayBuffer());

    return HttpResponse.arrayBuffer(imageBuffer, {
      headers: {
        'Content-Length': imageBuffer.byteLength.toString(),
        'Content-Type': 'image/svg+xml'
      }
    });
	}),
	http.get(umbracoPath('/security/back-office/graphics/logo-alternative'), async () => {
		const imageBuffer = await fetch(logoAlternativeUrl)
			.then((res) => res.arrayBuffer());

    return HttpResponse.arrayBuffer(imageBuffer, {
      headers: {
        'Content-Length': imageBuffer.byteLength.toString(),
        'Content-Type': 'image/svg+xml'
      }
    });
	}),
	http.get(umbracoPath('/security/back-office/graphics/background'), async () => {
		const imageBuffer = await fetch(backgroundUrl)
			.then((res) => res.arrayBuffer());

    return HttpResponse.arrayBuffer(imageBuffer, {
      headers: {
        'Content-Length': imageBuffer.byteLength.toString(),
        'Content-Type': 'image/jpeg'
      }
    });
	}),
];
