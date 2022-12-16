import './workspace-entity-layout.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbWorkspaceEntityLayout } from './workspace-entity-layout.element';

export default {
	title: 'Workspaces/Shared/Editor Entity Layout',
	component: 'umb-workspace-entity-layout',
	id: 'umb-workspace-entity-layout',
} as Meta;

export const AAAOverview: Story<UmbWorkspaceEntityLayout> = () => html` <umb-workspace-entity-layout>
	<div slot="icon"><uui-button color="" look="placeholder">Icon slot</uui-button></div>
	<div slot="name"><uui-button color="" look="placeholder">Name slot</uui-button></div>
	<div slot="footer"><uui-button color="" look="placeholder">Footer slot</uui-button></div>
	<div slot="actions"><uui-button color="" look="placeholder">Actions slot</uui-button></div>
	<uui-button color="" look="placeholder">Default slot</uui-button>
</umb-workspace-entity-layout>`;
AAAOverview.storyName = 'Overview';
