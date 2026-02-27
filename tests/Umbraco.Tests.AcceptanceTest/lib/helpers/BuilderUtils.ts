/**
 * Utility functions for builder classes to reduce code duplication.
 * These functions are designed to be called from within builder methods
 * without changing the builder's external API.
 */

/**
 * Ensures an ID/key field is set, generating a UUID if not provided.
 * Uses consistent null-check pattern across all builders.
 *
 * @param currentValue - The current value of the id/key field
 * @returns The existing value if set, or a new UUID
 */
export function ensureIdExists(currentValue: string | undefined | null): string {
  if (currentValue == null) {
    const crypto = require('crypto');
    return crypto.randomUUID();
  }
  return currentValue;
}

/**
 * Builds a property validation object with consistent defaults.
 * Uses null as the standard default for optional message fields.
 *
 * @param validation - Object containing validation properties
 * @returns Standardized validation object
 */
export function buildValidation(validation: {
  mandatory?: boolean;
  mandatoryMessage?: string;
  regEx?: string;
  regExMessage?: string;
}): {
  mandatory: boolean;
  mandatoryMessage: string | null;
  regEx: string | null;
  regExMessage: string | null;
} {
  return {
    mandatory: validation.mandatory || false,
    mandatoryMessage: validation.mandatoryMessage || null,
    regEx: validation.regEx || null,
    regExMessage: validation.regExMessage || null
  };
}

/**
 * Builds a container object with consistent defaults.
 *
 * @param container - Object containing container properties
 * @returns Standardized container object
 */
export function buildContainer(container: {
  id?: string;
  parentId?: string;
  name?: string;
  type?: string;
  sortOrder?: number;
}): {
  id: string | null;
  parent: { id: string } | null;
  name: string;
  type: string;
  sortOrder: number;
} {
  return {
    id: container.id || null,
    parent: container.parentId ? { id: container.parentId } : null,
    name: container.name || '',
    type: container.type || 'Group',
    sortOrder: container.sortOrder || 0
  };
}

/**
 * Builds a property object for content type builders with consistent defaults.
 *
 * @param property - Object containing property definition
 * @returns Standardized property object
 */
export function buildProperty(property: {
  id: string;
  containerId?: string;
  sortOrder?: number;
  alias?: string;
  name?: string;
  description?: string;
  dataTypeId?: string;
  variesByCulture?: boolean;
  variesBySegment?: boolean;
  mandatory?: boolean;
  mandatoryMessage?: string;
  regEx?: string;
  regExMessage?: string;
  labelOnTop?: boolean;
}): {
  id: string;
  container: { id: string | null };
  sortOrder: number;
  alias: string;
  name: string;
  description: string;
  dataType: { id: string | null };
  variesByCulture: boolean;
  variesBySegment: boolean;
  validation: {
    mandatory: boolean;
    mandatoryMessage: string | null;
    regEx: string | null;
    regExMessage: string | null;
  };
  appearance: { labelOnTop: boolean };
} {
  return {
    id: property.id,
    container: {
      id: property.containerId || null
    },
    sortOrder: property.sortOrder || 0,
    alias: property.alias || '',
    name: property.name || '',
    description: property.description || '',
    dataType: {
      id: property.dataTypeId || null
    },
    variesByCulture: property.variesByCulture || false,
    variesBySegment: property.variesBySegment || false,
    validation: buildValidation({
      mandatory: property.mandatory,
      mandatoryMessage: property.mandatoryMessage,
      regEx: property.regEx,
      regExMessage: property.regExMessage
    }),
    appearance: {
      labelOnTop: property.labelOnTop || false
    }
  };
}

/**
 * Builds a composition reference object with consistent defaults.
 *
 * @param typeIdField - The field name for the type ID (e.g., 'documentType', 'mediaType', 'memberType')
 * @param typeId - The ID value
 * @param compositionType - The composition type string
 * @returns Standardized composition object
 */
export function buildComposition(
  typeIdField: string,
  typeId: string | undefined,
  compositionType?: string
): {
  [key: string]: { id: string | null } | string;
  compositionType: string;
} {
  return {
    [typeIdField]: {
      id: typeId || null
    },
    compositionType: compositionType || 'Composition'
  };
}
