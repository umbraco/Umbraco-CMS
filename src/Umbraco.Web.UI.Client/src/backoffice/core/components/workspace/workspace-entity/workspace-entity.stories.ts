import './workspace-entity.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbWorkspaceEntity } from './workspace-entity.element';

export default {
	title: 'Workspaces/Shared/Editor Entity Layout',
	component: 'umb-workspace-entity',
	id: 'umb-workspace-entity',
} as Meta;

export const AAAOverview: Story<UmbWorkspaceEntity> = () => html` <umb-workspace-entity>
	<div slot="icon"><uui-button color="" look="placeholder">Icon slot</uui-button></div>
	<div slot="name"><uui-button color="" look="placeholder">Name slot</uui-button></div>
	<div slot="footer"><uui-button color="" look="placeholder">Footer slot</uui-button></div>
	<div slot="actions"><uui-button color="" look="placeholder">Actions slot</uui-button></div>
	<uui-button color="" look="placeholder">Default slot</uui-button>
</umb-workspace-entity>`;
AAAOverview.storyName = 'Overview';
