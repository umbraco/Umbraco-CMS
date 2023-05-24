import './extension-root-workspace.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import type { UmbExtensionRootWorkspaceElement } from './extension-root-workspace.element.js';

export default {
	title: 'Workspaces/Extensions',
	component: 'umb-workspace-extension-root',
	id: 'umb-workspace-extension-root',
} as Meta;

export const AAAOverview: Story<UmbExtensionRootWorkspaceElement> = () =>
	html` <umb-workspace-extension-root></umb-workspace-extension-root>`;
AAAOverview.storyName = 'Overview';
