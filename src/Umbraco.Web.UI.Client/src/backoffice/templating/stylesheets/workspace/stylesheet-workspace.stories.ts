import './stylesheet-workspace.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbStylesheetWorkspaceElement } from './stylesheet-workspace.element';

export default {
	title: 'Workspaces/Stylesheet',
	component: 'umb-stylesheet-workspace',
	id: 'umb-stylesheet-workspace',
} as Meta;

export const AAAOverview: Story<UmbStylesheetWorkspaceElement> = () =>
	html` <umb-stylesheet-workspace></umb-stylesheet-workspace>`;
AAAOverview.storyName = 'Overview';
