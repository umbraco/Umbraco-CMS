import { css } from '@umbraco-cms/backoffice/external/lit';

/**
 * Shared styles for templating workspace editors (templates, partial views, scripts).
 */
export const UMB_TEMPLATING_WORKSPACE_EDITOR_STYLES = css`
	:host {
		display: block;
		width: 100%;
		height: 100%;
	}

	umb-code-editor {
		--editor-height: calc(100dvh - 300px);
	}

	uui-box {
		min-height: calc(100dvh - 300px);
		margin: var(--uui-size-layout-1);
		--uui-box-default-padding: 0;
		/* remove header border bottom as code editor looks better in this box */
		--uui-color-divider-standalone: transparent;
	}

	#code-editor-menu-container {
		display: flex;
		gap: var(--uui-size-space-3);
	}

	#code-editor-menu-container uui-icon:not([name='icon-delete']) {
		margin-right: var(--uui-size-space-3);
	}
`;

/**
 * Styles for the production mode warning banner.
 */
export const UMB_PRODUCTION_MODE_WARNING_STYLES = css`
	#production-mode-warning {
		display: flex;
		align-items: center;
		gap: var(--uui-size-space-2);
		margin: var(--uui-size-layout-1);
		margin-bottom: 0;
		padding: var(--uui-size-space-3) var(--uui-size-space-4);
		background-color: var(--uui-color-warning);
		color: var(--uui-color-warning-contrast);
		border-radius: var(--uui-border-radius);
		font-size: var(--uui-type-small-size);
	}
`;
