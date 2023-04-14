import '../workspace-layout/workspace-layout.element';
import './workspace-footer-layout.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbWorkspaceFooterLayoutElement } from './workspace-footer-layout.element';

export default {
	title: 'Workspaces/Shared/Footer Layout',
	component: 'umb-workspace-footer-layout',
	id: 'umb-workspace-footer-layout',
} as Meta;

export const AAAOverview: Story<UmbWorkspaceFooterLayoutElement> = () => html` <umb-workspace-footer-layout>
	<div><uui-button color="" look="placeholder">Footer slot</uui-button></div>
	<div slot="actions"><uui-button color="" look="placeholder">Actions slot</uui-button></div>
</umb-workspace-footer-layout>`;
AAAOverview.storyName = 'Overview';
