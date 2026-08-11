/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Components/**/*.{razor,html,cshtml}",
    "./Pages/**/*.{razor,html,cshtml}"
  ],
  darkMode: "class",
  theme: {
    extend: {
      colors: {
        "on-secondary-container": "#0c6e68",
        "inverse-on-surface": "#edf0ff",
        "outline": "#737685",
        "primary-fixed": "#dae2ff",
        "surface-container": "#e8edff",
        "inverse-primary": "#b2c5ff",
        "primary-fixed-dim": "#b2c5ff",
        "primary": "#003d9a",
        "surface-container-low": "#f1f3ff",
        "on-tertiary-fixed-variant": "#812800",
        "surface-bright": "#f9f9ff",
        "error-container": "#ffdad6",
        "tertiary-fixed-dim": "#ffb59b",
        "background": "#f9f9ff",
        "secondary": "#006a64",
        "primary-container": "#2a56b4",
        "surface-variant": "#d7e2ff",
        "secondary-container": "#9deee6",
        "inverse-surface": "#1d3052",
        "error": "#ba1a1a",
        "on-primary": "#ffffff",
        "tertiary-container": "#a33500",
        "on-tertiary-fixed": "#380d00",
        "surface-container-high": "#e0e8ff",
        "surface-container-highest": "#d7e2ff",
        "on-secondary": "#ffffff",
        "on-error": "#ffffff",
        "tertiary": "#7b2600",
        "secondary-fixed-dim": "#84d5cd",
        "secondary-fixed": "#a0f1e9",
        "surface-container-lowest": "#ffffff",
        "on-surface-variant": "#434654",
        "on-background": "#041b3c",
        "on-error-container": "#93000a",
        "on-primary-fixed": "#001848",
        "on-surface": "#041b3c",
        "on-tertiary-container": "#ffc6b2",
        "on-secondary-fixed-variant": "#00504b",
        "on-primary-container": "#c4d2ff",
        "outline-variant": "#c3c6d6",
        "on-tertiary": "#ffffff",
        "surface": "#f9f9ff",
        "on-primary-fixed-variant": "#08409e",
        "on-secondary-fixed": "#00201e",
        "surface-dim": "#cadaff",
        "tertiary-fixed": "#ffdbcf",
        "surface-tint": "#2f59b7"
      },
      borderRadius: {
        "DEFAULT": "0.25rem",
        "lg": "0.5rem",
        "xl": "0.75rem",
        "full": "9999px"
      },
      spacing: {
        "gutter": "16px",
        "base-unit": "4px",
        "xl": "32px",
        "md": "16px",
        "sm": "8px",
        "container-margin-desktop": "40px",
        "container-margin-mobile": "16px",
        "xs": "4px",
        "lg": "24px"
      },
      fontFamily: {
        "label-sm": ["Inter", "sans-serif"],
        "label-md": ["Inter", "sans-serif"],
        "display-lg": ["Inter", "sans-serif"],
        "headline-md": ["Inter", "sans-serif"],
        "headline-lg": ["Inter", "sans-serif"],
        "body-lg": ["Inter", "sans-serif"],
        "body-md": ["Inter", "sans-serif"]
      },
      fontSize: {
        "label-sm": ["12px", { lineHeight: "16px", letterSpacing: "0.02em", fontWeight: "500" }],
        "label-md": ["14px", { lineHeight: "20px", letterSpacing: "0.01em", fontWeight: "600" }],
        "display-lg": ["48px", { lineHeight: "56px", letterSpacing: "-0.02em", fontWeight: "700" }],
        "headline-md": ["24px", { lineHeight: "32px", fontWeight: "600" }],
        "headline-lg": ["32px", { lineHeight: "40px", letterSpacing: "-0.01em", fontWeight: "600" }],
        "body-lg": ["18px", { lineHeight: "28px", fontWeight: "400" }],
        "body-md": ["16px", { lineHeight: "24px", fontWeight: "400" }]
      }
    }
  },
  plugins: [
    require("@tailwindcss/forms")
  ],
}
