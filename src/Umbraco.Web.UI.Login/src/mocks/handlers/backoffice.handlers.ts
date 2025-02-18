import { HttpHandler, http, HttpResponse } from "msw";
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import logoUrl from '../../../../Umbraco.Cms.StaticAssets/wwwroot/umbraco/assets/logo.svg';
import loginLogoUrl from '../../../../Umbraco.Cms.StaticAssets/wwwroot/umbraco/assets/logo_light.svg';
import loginLogoAlternativeUrl from '../../../../Umbraco.Cms.StaticAssets/wwwroot/umbraco/assets/logo_dark.svg';
import loginBackgroundUrl from '../../../../Umbraco.Cms.StaticAssets/wwwroot/umbraco/assets/login.jpg';

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
	http.get(umbracoPath('/security/back-office/graphics/login-logo'), async () => {
		const imageBuffer = await fetch(loginLogoUrl)
			.then((res) => res.arrayBuffer());

    return HttpResponse.arrayBuffer(imageBuffer, {
      headers: {
        'Content-Length': imageBuffer.byteLength.toString(),
        'Content-Type': 'image/svg+xml'
      }
    });
	}),
	http.get(umbracoPath('/security/back-office/graphics/login-logo-alternative'), async () => {
		const imageBuffer = await fetch(loginLogoAlternativeUrl)
			.then((res) => res.arrayBuffer());

    return HttpResponse.arrayBuffer(imageBuffer, {
      headers: {
        'Content-Length': imageBuffer.byteLength.toString(),
        'Content-Type': 'image/svg+xml'
      }
    });
	}),
	http.get(umbracoPath('/security/back-office/graphics/login-background'), async () => {
		const imageBuffer = await fetch(loginBackgroundUrl)
			.then((res) => res.arrayBuffer());

    return HttpResponse.arrayBuffer(imageBuffer, {
      headers: {
        'Content-Length': imageBuffer.byteLength.toString(),
        'Content-Type': 'image/jpeg'
      }
    });
	}),
];
