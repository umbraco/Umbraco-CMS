import './relation-type-workspace.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import { ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { data } from '../../../../mocks/data/relation-type.data.js';

import type { UmbRelationTypeWorkspaceElement } from './relation-type-workspace.element.js';

export default {
	title: 'Workspaces/Relation Type',
	component: 'umb-relation-type-workspace',
	id: 'umb-relation-type-workspace',
} as Meta;

export const AAAOverview: Story<UmbRelationTypeWorkspaceElement> = () =>
	html` <umb-relation-type-workspace id="${ifDefined(data[0].id)}"></umb-relation-type-workspace>`;
AAAOverview.storyName = 'Overview';
