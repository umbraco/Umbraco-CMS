import './workspace-editor.element.js';

import type { UmbWorkspaceEditorElement } from './workspace-editor.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'Workspaces/Shared/Workspace Editor',
	component: 'umb-workspace-editor',
	id: 'umb-workspace-editor',
} as Meta;

export const AAAOverview: StoryFn<UmbWorkspaceEditorElement> = () =>
	html` <umb-workspace-editor>
		<div slot="icon"><uui-button color="" look="placeholder">Icon slot</uui-button></div>
		<div slot="name"><uui-button color="" look="placeholder">Name slot</uui-button></div>
		<div slot="footer-info"><uui-button color="" look="placeholder">Footer slot</uui-button></div>
		<div slot="actions"><uui-button color="" look="placeholder">Actions slot</uui-button></div>
		<uui-button color="" look="placeholder">Default slot</uui-button>
	</umb-workspace-editor>`;
AAAOverview.storyName = 'Overview';
