import './stylesheet-workspace.element.js';

import { Meta, Story } from '@storybook/web-components';
import type { UmbStylesheetWorkspaceElement } from './stylesheet-workspace.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';


export default {
	title: 'Workspaces/Stylesheet',
	component: 'umb-stylesheet-workspace',
	id: 'umb-stylesheet-workspace',
} as Meta;

export const AAAOverview: Story<UmbStylesheetWorkspaceElement> = () =>
	html` <umb-stylesheet-workspace></umb-stylesheet-workspace>`;
AAAOverview.storyName = 'Overview';
