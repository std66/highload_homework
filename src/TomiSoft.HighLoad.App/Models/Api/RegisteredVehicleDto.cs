/*
 * Vehicle registration API
 *
 * A service to manage vehicle registrations.
 *
 * OpenAPI spec version: 1.0.1
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace TomiSoft.HighLoad.App.Models.Api {
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public partial class RegisteredVehicleDto : IEquatable<RegisteredVehicleDto> {
        /// <summary>
        /// The vehicle&#x27;s unique ID in this system
        /// </summary>
        /// <value>The vehicle&#x27;s unique ID in this system</value>
        [Required]

        [DataMember(Name = "uuid")]
        public required Guid? Uuid { get; set; }

        /// <summary>
        /// Vehicle license plate number
        /// </summary>
        /// <value>Vehicle license plate number</value>
        [Required]

        [StringLength(20, MinimumLength = 1)]
        [DataMember(Name = "rendszam")]
        public required string Rendszam { get; set; }

        /// <summary>
        /// Vehicle owner
        /// </summary>
        /// <value>Vehicle owner</value>
        [Required]

        [StringLength(200, MinimumLength = 1)]
        [DataMember(Name = "tulajdonos")]
        public required string Tulajdonos { get; set; }

        /// <summary>
        /// Validity of traffic permit
        /// </summary>
        /// <value>Validity of traffic permit</value>
        [Required]

        [DataMember(Name = "forgalmi_ervenyes")]
        public required DateOnly? ForgalmiErvenyes { get; set; }

        /// <summary>
        /// Additional information
        /// </summary>
        /// <value>Additional information</value>
        [Required]

        [DataMember(Name = "adatok")]
        public required List<string> Adatok { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("class RegisteredVehicleDto {\n");
            sb.Append("  Uuid: ").Append(Uuid).Append("\n");
            sb.Append("  Rendszam: ").Append(Rendszam).Append("\n");
            sb.Append("  Tulajdonos: ").Append(Tulajdonos).Append("\n");
            sb.Append("  ForgalmiErvenyes: ").Append(ForgalmiErvenyes).Append("\n");
            sb.Append("  Adatok: ").Append(Adatok).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object? obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((RegisteredVehicleDto)obj);
        }

        /// <summary>
        /// Returns true if RegisteredVehicleDto instances are equal
        /// </summary>
        /// <param name="other">Instance of RegisteredVehicleDto to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(RegisteredVehicleDto? other) {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return
                (
                    Uuid == other.Uuid ||
                    Uuid != null &&
                    Uuid.Equals(other.Uuid)
                ) &&
                (
                    Rendszam == other.Rendszam ||
                    Rendszam != null &&
                    Rendszam.Equals(other.Rendszam)
                ) &&
                (
                    Tulajdonos == other.Tulajdonos ||
                    Tulajdonos != null &&
                    Tulajdonos.Equals(other.Tulajdonos)
                ) &&
                (
                    ForgalmiErvenyes == other.ForgalmiErvenyes ||
                    ForgalmiErvenyes != null &&
                    ForgalmiErvenyes.Equals(other.ForgalmiErvenyes)
                ) &&
                (
                    Adatok == other.Adatok ||
                    Adatok != null &&
                    Adatok.SequenceEqual(other.Adatok)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode() {
            unchecked // Overflow is fine, just wrap
            {
                var hashCode = 41;
                // Suitable nullity checks etc, of course :)
                if (Uuid != null)
                    hashCode = hashCode * 59 + Uuid.GetHashCode();
                if (Rendszam != null)
                    hashCode = hashCode * 59 + Rendszam.GetHashCode();
                if (Tulajdonos != null)
                    hashCode = hashCode * 59 + Tulajdonos.GetHashCode();
                if (ForgalmiErvenyes != null)
                    hashCode = hashCode * 59 + ForgalmiErvenyes.GetHashCode();
                if (Adatok != null)
                    hashCode = hashCode * 59 + Adatok.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
#pragma warning disable 1591

        public static bool operator ==(RegisteredVehicleDto left, RegisteredVehicleDto right) {
            return Equals(left, right);
        }

        public static bool operator !=(RegisteredVehicleDto left, RegisteredVehicleDto right) {
            return !Equals(left, right);
        }

#pragma warning restore 1591
        #endregion Operators
    }
}
