import './member-workspace.element.js';

import type { Meta, Story } from '@storybook/web-components';

import type { UmbMemberWorkspaceElement } from './member-workspace.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'Workspaces/Member',
	component: 'umb-member-workspace',
	id: 'umb-member-workspace',
} as Meta;

export const AAAOverview: Story<UmbMemberWorkspaceElement> = () =>
	html` <!--
	<umb-member-workspace"></umb-member-workspace>
-->`;
AAAOverview.storyName = 'Overview';
