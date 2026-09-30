import { css } from '@umbraco-cms/backoffice/external/lit';

export const exampleBlockViewStyles = css`
	:host {
		display: block;
		height: 100%;
		box-sizing: border-box;
		overflow: hidden;
		background-color: var(--uui-color-surface);
		color: var(--uui-color-text);
		font-family: 'Helvetica Neue', Helvetica, Arial, system-ui, sans-serif;
		-webkit-font-smoothing: antialiased;
	}

	h2,
	h3,
	h4,
	p {
		margin: 0;
	}

	img {
		display: block;
		width: 100%;
		object-fit: cover;
		transition: transform 0.6s cubic-bezier(0.2, 0.7, 0.2, 1);
	}

	:host(:hover) img {
		transform: scale(1.04);
	}

	.media {
		overflow: hidden;
		background-color: var(--uui-color-surface-alt);
	}

	.body {
		display: flex;
		flex-direction: column;
		align-items: flex-start;
		gap: var(--uui-size-space-3);
		padding: var(--uui-size-space-5);
	}

	.eyebrow {
		font-size: 0.6875rem;
		font-weight: 700;
		letter-spacing: 0.14em;
		text-transform: uppercase;
		color: var(--uui-color-text-alt);
	}

	.title {
		font-size: 1.375rem;
		font-weight: 800;
		letter-spacing: -0.02em;
		line-height: 1.15;
	}

	.lead {
		color: var(--uui-color-text-alt);
		line-height: 1.5;
		display: -webkit-box;
		-webkit-line-clamp: 3;
		-webkit-box-orient: vertical;
		overflow: hidden;
	}

	.pill {
		display: inline-block;
		padding: 0.3em 0.9em;
		border-radius: 999px;
		background-color: var(--uui-color-text);
		color: var(--uui-color-surface);
		font-size: 0.6875rem;
		font-weight: 700;
		letter-spacing: 0.1em;
		text-transform: uppercase;
	}

	.cta {
		display: inline-flex;
		gap: 0.6em;
		padding-bottom: 2px;
		border-bottom: 2px solid currentColor;
		font-size: 0.875rem;
		font-weight: 700;
	}
`;
