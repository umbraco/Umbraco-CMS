import './workspace-layout.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbWorkspaceLayoutElement } from './workspace-layout.element';

export default {
	title: 'Workspaces/Shared/Editor Entity Layout',
	component: 'umb-workspace-layout',
	id: 'umb-workspace-layout',
} as Meta;

export const AAAOverview: Story<UmbWorkspaceLayoutElement> = () => html` <umb-workspace-layout>
	<div slot="icon"><uui-button color="" look="placeholder">Icon slot</uui-button></div>
	<div slot="name"><uui-button color="" look="placeholder">Name slot</uui-button></div>
	<div slot="footer"><uui-button color="" look="placeholder">Footer slot</uui-button></div>
	<div slot="actions"><uui-button color="" look="placeholder">Actions slot</uui-button></div>
	<uui-button color="" look="placeholder">Default slot</uui-button>
</umb-workspace-layout>`;
AAAOverview.storyName = 'Overview';
