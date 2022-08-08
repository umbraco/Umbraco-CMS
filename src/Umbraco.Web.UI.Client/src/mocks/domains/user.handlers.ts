import { rest } from 'msw';

import umbracoPath from '../../core/helpers/umbraco-path';
import { AllowedSectionsResponse, UserResponse } from '../../core/models';

let isAuthenticated = false;

export const handlers = [
	rest.post(umbracoPath('/user/login'), (_req, res, ctx) => {
		// Persist user's authentication in the session
		isAuthenticated = true;
		return res(
			// Respond with a 200 status code
			ctx.status(201)
		);
	}),

	rest.post(umbracoPath('/user/logout'), (_req, res, ctx) => {
		// Persist user's authentication in the session
		isAuthenticated = false;
		return res(
			// Respond with a 200 status code
			ctx.status(201)
		);
	}),

	rest.get(umbracoPath('/user'), (_req, res, ctx) => {
		// Check if the user is authenticated in this session
		if (!isAuthenticated) {
			// If not authenticated, respond with a 403 error
			return res(
				ctx.status(403),
				ctx.json({
					errorMessage: 'Not authorized',
				})
			);
		}
		// If authenticated, return a mocked user details
		return res(
			ctx.status(200),
			ctx.json<UserResponse>({
				username: 'admin',
				role: 'administrator',
			})
		);
	}),

	rest.get(umbracoPath('/user/sections'), (_req, res, ctx) => {
		return res(
			ctx.status(200),
			ctx.json<AllowedSectionsResponse>({
				sections: ['Umb.Section.Content', 'Umb.Section.Settings', 'My.Section.Custom'],
			})
		);
	}),
];
