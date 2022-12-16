import './editor-extensions.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbWorkspaceExtensionsElement } from './editor-extensions.element';

export default {
	title: 'Editors/Extensions',
	component: 'umb-workspace-extensions',
	id: 'umb-workspace-extensions',
} as Meta;

export const AAAOverview: Story<UmbWorkspaceExtensionsElement> = () =>
	html` <umb-workspace-extensions></umb-workspace-extensions>`;
AAAOverview.storyName = 'Overview';
