import './workspace-extension-root.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbWorkspaceExtensionRootElement } from './workspace-extension-root.element';

export default {
	title: 'Workspaces/Extensions',
	component: 'umb-workspace-extension-root',
	id: 'umb-workspace-extension-root',
} as Meta;

export const AAAOverview: Story<UmbWorkspaceExtensionRootElement> = () =>
	html` <umb-workspace-extension-root></umb-workspace-extension-root>`;
AAAOverview.storyName = 'Overview';
