import './extension-root-workspace.element.js';

import { Meta, Story } from '@storybook/web-components';
import type { UmbExtensionRootWorkspaceElement } from './extension-root-workspace.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';


export default {
	title: 'Workspaces/Extensions',
	component: 'umb-workspace-extension-root',
	id: 'umb-workspace-extension-root',
} as Meta;

export const AAAOverview: Story<UmbExtensionRootWorkspaceElement> = () =>
	html` <umb-workspace-extension-root></umb-workspace-extension-root>`;
AAAOverview.storyName = 'Overview';
