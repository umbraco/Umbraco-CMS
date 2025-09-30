import '../workspace-editor/workspace-editor.element.js';
import './workspace-footer.element.js';

import type { UmbWorkspaceFooterLayoutElement } from './workspace-footer.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'Extension Type/Workspace/Components/Workspace Footer',
	component: 'umb-workspace-footer',
	id: 'umb-workspace-footer',
} as Meta;

export const Docs: StoryFn<UmbWorkspaceFooterLayoutElement> = () =>
	html` <umb-workspace-footer>
		<div><uui-button color="" look="placeholder">Footer slot</uui-button></div>
		<div slot="actions"><uui-button color="" look="placeholder">Actions slot</uui-button></div>
	</umb-workspace-footer>`;
