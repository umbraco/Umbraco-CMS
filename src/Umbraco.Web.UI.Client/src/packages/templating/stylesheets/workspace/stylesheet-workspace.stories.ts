import './stylesheet-workspace.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import type { UmbStylesheetWorkspaceElement } from './stylesheet-workspace.element.js';

export default {
	title: 'Workspaces/Stylesheet',
	component: 'umb-stylesheet-workspace',
	id: 'umb-stylesheet-workspace',
} as Meta;

export const AAAOverview: Story<UmbStylesheetWorkspaceElement> = () =>
	html` <umb-stylesheet-workspace></umb-stylesheet-workspace>`;
AAAOverview.storyName = 'Overview';
