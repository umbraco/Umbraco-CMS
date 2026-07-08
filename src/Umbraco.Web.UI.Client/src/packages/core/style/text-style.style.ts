import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css } from '@umbraco-cms/backoffice/external/lit';

export const UmbTextStyles = css`
	${UUITextStyles}

	a {
		color: var(--uui-color-interactive);
	}
	a:hover,
	a:focus {
		color: var(--uui-color-interactive-emphasis);
	}

	hr {
		border: 0;
		border-top: 1px solid var(--uui-color-border);
	}

	.sr-only {
		position: absolute;
		width: 1px;
		height: 1px;
		padding: 0;
		margin: -1px;
		overflow: hidden;
		clip: rect(0, 0, 0, 0);
		white-space: nowrap;
		border: 0;
	}
`;
