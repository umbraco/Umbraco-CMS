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
`;
