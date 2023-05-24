import '../workspace-editor/workspace-editor.element';
import './workspace-footer.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import type { UmbWorkspaceFooterLayoutElement } from './workspace-footer.element.js';

export default {
	title: 'Workspaces/Shared/Footer Layout',
	component: 'umb-workspace-footer',
	id: 'umb-workspace-footer',
} as Meta;

export const AAAOverview: Story<UmbWorkspaceFooterLayoutElement> = () => html` <umb-workspace-footer>
	<div><uui-button color="" look="placeholder">Footer slot</uui-button></div>
	<div slot="actions"><uui-button color="" look="placeholder">Actions slot</uui-button></div>
</umb-workspace-footer>`;
AAAOverview.storyName = 'Overview';
