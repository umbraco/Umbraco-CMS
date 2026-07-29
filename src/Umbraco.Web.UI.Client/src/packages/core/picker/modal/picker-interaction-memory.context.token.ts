import type { UmbInteractionMemoryManager } from '@umbraco-cms/backoffice/interaction-memory';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

/**
 * Resolves to the nearest `UmbInteractionMemoryManager` acting as the interaction-memory scope for a
 * picker modal — e.g. a `UmbPickerInputContext` or a property editor's own
 * `UmbPropertyEditorUiInteractionMemoryManager`. Distinct from `UMB_INTERACTION_MEMORY_CONTEXT`, which
 * is a single page-wide store: this token lets each provider apply its own scoping (selection,
 * property-editor configuration) before the modal's memory reaches that store.
 */
export const UMB_PICKER_INTERACTION_MEMORY_CONTEXT = new UmbContextToken<UmbInteractionMemoryManager>(
	'UmbPickerInteractionMemory',
);
