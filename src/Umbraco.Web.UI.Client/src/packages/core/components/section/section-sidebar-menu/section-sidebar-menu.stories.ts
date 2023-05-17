import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbSectionSidebarMenuElement } from './section-sidebar-menu.element';
import './section-sidebar-menu.element';

export default {
	title: 'Sections/Shared/Section Sidebar Menu',
	component: 'umb-section-sidebar-menu',
	id: 'umb-section-sidebar-menu',
} as Meta;

export const AAAOverview: Story<UmbSectionSidebarMenuElement> = () =>
	html` <umb-section-sidebar-menu></umb-section-sidebar-menu>`;
AAAOverview.storyName = 'Overview';
