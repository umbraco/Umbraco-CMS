import { rest } from 'msw';
import { AllowedSectionsResponse, UserResponse } from '../../core/models';

export const handlers = [
	rest.post('/umbraco/backoffice/user/login', (_req, res, ctx) => {
		// Persist user's authentication in the session
		sessionStorage.setItem('is-authenticated', 'true');
		return res(
			// Respond with a 200 status code
			ctx.status(201)
		);
	}),

	rest.post('/umbraco/backoffice/user/logout', (_req, res, ctx) => {
		// Persist user's authentication in the session
		sessionStorage.removeItem('is-authenticated');
		return res(
			// Respond with a 200 status code
			ctx.status(201)
		);
	}),

	rest.get('/umbraco/backoffice/user', (_req, res, ctx) => {
		// Check if the user is authenticated in this session
		const isAuthenticated = sessionStorage.getItem('is-authenticated');
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

	rest.get('/umbraco/backoffice/user/sections', (_req, res, ctx) => {
		return res(
			ctx.status(200),
			ctx.json<AllowedSectionsResponse>({
				sections: ['Umb.Section.Content', 'Umb.Section.Media', 'Umb.Section.Settings', 'My.Section.Custom'],
			})
		);
	}),
];
